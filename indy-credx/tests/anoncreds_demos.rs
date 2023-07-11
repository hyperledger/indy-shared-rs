use std::collections::HashMap;

use indy_credx::{
    issuer, prover,
    tails::{TailsFileReader, TailsFileWriter},
    types::{
        CredentialDefinitionConfig, CredentialRevocationConfig, IssuanceType, MakeCredentialValues,
        PresentCredentials, RegistryType, RevocationRegistryDefinition, SignatureType,
    },
    verifier,
};

use serde_json::json;

use self::utils::anoncreds::{IssuerWallet, ProverWallet};

mod utils;

pub static GVT_SCHEMA_NAME: &'static str = "gvt";
pub static GVT_SCHEMA_ATTRIBUTES: &[&'static str; 4] = &["name", "age", "sex", "height"];

#[test]
fn anoncreds_works_for_single_issuer_single_prover() {
    // Create Issuer pseudo wallet
    let mut issuer_wallet = IssuerWallet::default();

    // Create Prover pseudo wallet and master secret
    let mut prover_wallet = ProverWallet::default();

    // Issuer creates Schema - would be published to the ledger
    let gvt_schema = issuer::create_schema(
        &issuer_wallet.did,
        GVT_SCHEMA_NAME,
        "1.0",
        GVT_SCHEMA_ATTRIBUTES[..].into(),
        None,
    )
    .expect("Error creating gvt schema for issuer");

    // Issuer creates Credential Definition
    let cred_def_parts = issuer::create_credential_definition(
        &issuer_wallet.did,
        &gvt_schema,
        "tag",
        SignatureType::CL,
        CredentialDefinitionConfig {
            support_revocation: false,
        },
    )
    .expect("Error creating gvt credential definition");
    issuer_wallet.cred_defs.push(cred_def_parts.into());

    // Public part would be published to the ledger
    let gvt_cred_def = &issuer_wallet.cred_defs[0].public;

    // Issuer creates a Credential Offer
    let cred_offer = issuer::create_credential_offer(
        gvt_schema.id(),
        &gvt_cred_def,
        &issuer_wallet.cred_defs[0].key_proof,
    )
    .expect("Error creating credential offer");

    // Prover creates a Credential Request
    let (cred_request, cred_request_metadata) = prover::create_credential_request(
        &prover_wallet.did,
        &*gvt_cred_def,
        &prover_wallet.master_secret,
        "default",
        &cred_offer,
    )
    .expect("Error creating credential request");

    // Issuer creates a credential
    let mut cred_values = MakeCredentialValues::default();
    cred_values
        .add_raw("sex", "male")
        .expect("Error encoding attribute");
    cred_values
        .add_raw("name", "Alex")
        .expect("Error encoding attribute");
    cred_values
        .add_raw("height", "175")
        .expect("Error encoding attribute");
    cred_values
        .add_raw("age", "28")
        .expect("Error encoding attribute");
    let (issue_cred, _, _) = issuer::create_credential(
        &*gvt_cred_def,
        &issuer_wallet.cred_defs[0].private,
        &cred_offer,
        &cred_request,
        cred_values.into(),
        None,
    )
    .expect("Error creating credential");

    // Prover receives the credential and processes it
    let mut recv_cred = issue_cred;
    prover::process_credential(
        &mut recv_cred,
        &cred_request_metadata,
        &prover_wallet.master_secret,
        &*gvt_cred_def,
        None,
    )
    .expect("Error processing credential");
    prover_wallet.credentials.push(recv_cred);

    // Verifier creates a presentation request
    let nonce = verifier::generate_nonce().expect("Error generating presentation request nonce");
    let pres_request = serde_json::from_value(json!({
        "nonce": nonce,
        "name":"pres_req_1",
        "version":"0.1",
        "requested_attributes":{
            "attr1_referent":{
                "name":"name"
            },
            "attr2_referent":{
                "name":"sex"
            },
            "attr3_referent":{"name":"phone"},
            "attr4_referent":{
                "names": ["name", "height"]
            }
        },
        "requested_predicates":{
            "predicate1_referent":{"name":"age","p_type":">=","p_value":18}
        }
    }))
    .expect("Error creating proof request");

    // TODO: show deriving the wallet query from the proof request (need to add helper)

    // Prover creates presentation
    let mut present = PresentCredentials::default();
    {
        let mut cred1 = present.add_credential(&prover_wallet.credentials[0], None, None);
        cred1.add_requested_attribute("attr1_referent", true);
        cred1.add_requested_attribute("attr2_referent", false);
        cred1.add_requested_attribute("attr4_referent", true);
        cred1.add_requested_predicate("predicate1_referent");
    }

    let mut self_attested = HashMap::new();
    let self_attested_phone = "8-800-300";
    self_attested.insert(
        "attr3_referent".to_string(),
        self_attested_phone.to_string(),
    );

    let mut schemas = HashMap::new();
    schemas.insert(gvt_schema.id().clone(), &gvt_schema);

    let mut cred_defs = HashMap::new();
    cred_defs.insert(gvt_cred_def.id().clone(), &*gvt_cred_def);

    let presentation = prover::create_presentation(
        &pres_request,
        present,
        Some(self_attested),
        &prover_wallet.master_secret,
        &schemas,
        &cred_defs,
    )
    .expect("Error creating presentation");

    // Verifier verifies presentation
    assert_eq!(
        "Alex",
        presentation
            .requested_proof
            .revealed_attrs
            .get("attr1_referent")
            .unwrap()
            .raw
    );
    assert_eq!(
        0,
        presentation
            .requested_proof
            .unrevealed_attrs
            .get("attr2_referent")
            .unwrap()
            .sub_proof_index
    );
    assert_eq!(
        self_attested_phone,
        presentation
            .requested_proof
            .self_attested_attrs
            .get("attr3_referent")
            .unwrap()
    );
    let revealed_attr_groups = presentation
        .requested_proof
        .revealed_attr_groups
        .get("attr4_referent")
        .unwrap();
    assert_eq!("Alex", revealed_attr_groups.values.get("name").unwrap().raw);
    assert_eq!(
        "175",
        revealed_attr_groups.values.get("height").unwrap().raw
    );
    let valid = verifier::verify_presentation(
        &presentation,
        &pres_request,
        &schemas,
        &cred_defs,
        None,
        None,
    )
    .expect("Error verifying presentation");
    assert!(valid);
}

#[test]
fn anoncreds_works_for_single_issuer_single_prover_unrevoked() {
    // Create Issuer pseudo wallet
    let mut issuer_wallet = IssuerWallet::default();

    // Create Prover pseudo wallet and master secret
    let mut prover_wallet = ProverWallet::default();

    // Issuer creates Schema - would be published to the ledger
    let gvt_schema = issuer::create_schema(
        &issuer_wallet.did,
        GVT_SCHEMA_NAME,
        "1.0",
        GVT_SCHEMA_ATTRIBUTES[..].into(),
        None,
    )
    .expect("Error creating gvt schema for issuer");

    // Issuer creates Credential Definition
    let cred_def_parts = issuer::create_credential_definition(
        &issuer_wallet.did,
        &gvt_schema,
        "tag",
        SignatureType::CL,
        CredentialDefinitionConfig {
            support_revocation: true,
        },
    )
    .expect("Error creating gvt credential definition");
    issuer_wallet.cred_defs.push(cred_def_parts.into());

    // Public part would be published to the ledger
    let gvt_cred_def = &issuer_wallet.cred_defs[0].public;

    // Create revocation registry definition and initial registry
    let mut tails_writer = TailsFileWriter::new(None);
    let (rev_reg_def, rev_reg_def_private, rev_reg, rev_reg_delta) =
        issuer::create_revocation_registry(
            &issuer_wallet.did,
            &gvt_cred_def,
            "tag",
            RegistryType::CL_ACCUM,
            IssuanceType::ISSUANCE_BY_DEFAULT,
            5,
            &mut tails_writer,
        )
        .expect("Error creating revocation registry definition");
    let tails_path = match rev_reg_def {
        RevocationRegistryDefinition::RevocationRegistryDefinitionV1(ref r) => {
            r.value.tails_location.to_string()
        }
    };

    // Issuer creates a Credential Offer
    let cred_offer = issuer::create_credential_offer(
        gvt_schema.id(),
        &gvt_cred_def,
        &issuer_wallet.cred_defs[0].key_proof,
    )
    .expect("Error creating credential offer");

    // Prover creates a Credential Request
    let (cred_request, cred_request_metadata) = prover::create_credential_request(
        &prover_wallet.did,
        &gvt_cred_def,
        &prover_wallet.master_secret,
        "default",
        &cred_offer,
    )
    .expect("Error creating credential request");

    // Issuer creates a credential
    let mut cred_values = MakeCredentialValues::default();
    cred_values
        .add_raw("sex", "male")
        .expect("Error encoding attribute");
    cred_values
        .add_raw("name", "Alex")
        .expect("Error encoding attribute");
    cred_values
        .add_raw("height", "175")
        .expect("Error encoding attribute");
    cred_values
        .add_raw("age", "28")
        .expect("Error encoding attribute");
    let (issue_cred, _rev_reg, _delta) = issuer::create_credential(
        &*gvt_cred_def,
        &issuer_wallet.cred_defs[0].private,
        &cred_offer,
        &cred_request,
        cred_values.into(),
        Some(CredentialRevocationConfig {
            reg_def: &rev_reg_def,
            reg_def_private: &rev_reg_def_private,
            registry: &rev_reg,
            registry_idx: 1,
            registry_used: &Default::default(),
        }),
    )
    .expect("Error creating credential");

    // Prover receives the credential and processes it
    let mut recv_cred = issue_cred;
    prover::process_credential(
        &mut recv_cred,
        &cred_request_metadata,
        &prover_wallet.master_secret,
        &*gvt_cred_def,
        Some(&rev_reg_def),
    )
    .expect("Error processing credential");
    prover_wallet.credentials.push(recv_cred);

    // Verifier creates a presentation request
    let nonce = verifier::generate_nonce().expect("Error generating presentation request nonce");
    let pres_request = serde_json::from_value(json!({
        "nonce": nonce,
        "name":"pres_req_1",
        "version":"0.1",
        "requested_attributes":{
            "attr1_referent":{
                "name":"name"
            },
            "attr2_referent":{
                "name":"sex"
            },
            "attr3_referent":{"name":"phone"},
            "attr4_referent":{
                "names": ["name", "height"]
            }
        },
        "requested_predicates":{
            "predicate1_referent":{"name":"age","p_type":">=","p_value":18}
        },
        "non_revoked": {"from": 1, "to": 1}
    }))
    .expect("Error creating proof request");

    // Prover creates revocation state
    let tails_reader = TailsFileReader::new(&tails_path);
    let rev_state = prover::create_or_update_revocation_state(
        tails_reader,
        &rev_reg_def,
        &rev_reg_delta,
        1,
        1,
        None,
    )
    .expect("Error creating revocation state");

    // Prover creates presentation
    let mut present = PresentCredentials::default();
    {
        let mut cred1 =
            present.add_credential(&prover_wallet.credentials[0], Some(1), Some(&rev_state));
        cred1.add_requested_attribute("attr1_referent", true);
        cred1.add_requested_attribute("attr2_referent", false);
        cred1.add_requested_attribute("attr4_referent", true);
        cred1.add_requested_predicate("predicate1_referent");
    }

    let mut self_attested = HashMap::new();
    let self_attested_phone = "8-800-300";
    self_attested.insert(
        "attr3_referent".to_string(),
        self_attested_phone.to_string(),
    );

    let mut schemas = HashMap::new();
    schemas.insert(gvt_schema.id().clone(), &gvt_schema);

    let mut cred_defs = HashMap::new();
    cred_defs.insert(gvt_cred_def.id().clone(), &*gvt_cred_def);

    let presentation = prover::create_presentation(
        &pres_request,
        present,
        Some(self_attested),
        &prover_wallet.master_secret,
        &schemas,
        &cred_defs,
    )
    .expect("Error creating presentation");

    // Verifier verifies presentation
    let mut reg_defs = HashMap::new();
    reg_defs.insert(rev_reg_def.id().clone(), &rev_reg_def);
    let mut rev_regs = HashMap::new();
    let mut rev_entry = HashMap::new();
    rev_entry.insert(1, &rev_reg);
    rev_regs.insert(rev_reg_def.id().clone(), rev_entry);
    let valid = verifier::verify_presentation(
        &presentation,
        &pres_request,
        &schemas,
        &cred_defs,
        Some(&reg_defs),
        Some(&rev_regs),
    )
    .expect("Error verifying presentation");
    assert!(valid);
}
