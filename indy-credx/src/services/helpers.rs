use std::collections::{HashMap, HashSet};

use indy_data_types::anoncreds::{
    credential::AttributeValues,
    nonce::Nonce,
    pres_request::{AttributeInfo, NonRevocedInterval, PredicateInfo},
};

use crate::anoncreds_clsignatures::{
    hash_credential_attribute, CredentialSchema, CredentialValues as CryptoCredentialValues,
    Issuer as ClIssuer, MasterSecret as CryptoMasterSecret, NonCredentialSchema, SubProofRequest,
    Verifier as ClVerifier,
};
use crate::error::Result;

pub fn attr_common_view(attr: &str) -> String {
    attr.replace(" ", "").to_lowercase()
}

pub fn build_credential_schema(attrs: &HashSet<String>) -> Result<CredentialSchema> {
    trace!("build_credential_schema >>> attrs: {:?}", attrs);

    let mut credential_schema_builder = ClIssuer::new_credential_schema_builder()?;
    for attr in attrs {
        credential_schema_builder.add_attr(&attr_common_view(attr))?;
    }

    let res = credential_schema_builder.finalize()?;

    trace!("build_credential_schema <<< res: {:?}", res);

    Ok(res)
}

pub fn build_non_credential_schema() -> Result<NonCredentialSchema> {
    trace!("build_non_credential_schema");

    let mut non_credential_schema_builder = ClIssuer::new_non_credential_schema_builder()?;
    non_credential_schema_builder.add_attr("master_secret")?;
    let res = non_credential_schema_builder.finalize()?;

    trace!("build_non_credential_schema <<< res: {:?}", res);
    Ok(res)
}

pub fn build_credential_values(
    credential_values: &HashMap<String, AttributeValues>,
    master_secret: Option<&CryptoMasterSecret>,
) -> Result<CryptoCredentialValues> {
    trace!(
        "build_credential_values >>> credential_values: {:?}",
        credential_values
    );

    let mut credential_values_builder = ClIssuer::new_credential_values_builder()?;
    for (attr, values) in credential_values {
        credential_values_builder.add_dec_known(&attr_common_view(attr), &values.encoded)?;
    }
    if let Some(ms) = master_secret {
        credential_values_builder.add_value_hidden("master_secret", &ms.value()?)?;
    }

    let res = credential_values_builder.finalize()?;

    trace!("build_credential_values <<< res: {:?}", res);

    Ok(res)
}

pub fn encode_credential_attribute(raw_value: &str) -> Result<String> {
    if let Ok(val) = raw_value.parse::<i32>() {
        Ok(val.to_string())
    } else {
        Ok(hash_credential_attribute(raw_value)?)
    }
}

pub fn build_sub_proof_request(
    attrs_for_credential: &[AttributeInfo],
    predicates_for_credential: &[PredicateInfo],
) -> Result<SubProofRequest> {
    trace!(
        "build_sub_proof_request >>> attrs_for_credential: {:?}, predicates_for_credential: {:?}",
        attrs_for_credential,
        predicates_for_credential
    );

    let mut sub_proof_request_builder = ClVerifier::new_sub_proof_request_builder()?;

    for attr in attrs_for_credential {
        let names = if let Some(name) = &attr.name {
            vec![name.clone()]
        } else if let Some(names) = &attr.names {
            names.to_owned()
        } else {
            error!(
                r#"Attr for credential restriction should contain "name" or "names" param. Current attr: {:?}"#,
                attr
            );
            return Err(err_msg!(
                r#"Attr for credential restriction should contain "name" or "names" param."#,
            ));
        };

        for name in names {
            sub_proof_request_builder.add_revealed_attr(&attr_common_view(&name))?
        }
    }

    for predicate in predicates_for_credential {
        let p_type = format!("{}", predicate.p_type);

        sub_proof_request_builder.add_predicate(
            &attr_common_view(&predicate.name),
            &p_type,
            predicate.p_value,
        )?;
    }

    let res = sub_proof_request_builder.finalize()?;

    trace!("build_sub_proof_request <<< res: {:?}", res);

    Ok(res)
}

pub fn get_non_revoc_interval(
    global_interval: &Option<NonRevocedInterval>,
    local_interval: &Option<NonRevocedInterval>,
) -> Option<NonRevocedInterval> {
    trace!(
        "get_non_revoc_interval >>> global_interval: {:?}, local_interval: {:?}",
        global_interval,
        local_interval
    );

    let interval = local_interval
        .clone()
        .or_else(|| global_interval.clone().or(None));

    trace!("get_non_revoc_interval <<< interval: {:?}", interval);

    interval
}

pub fn new_nonce() -> Result<Nonce> {
    Nonce::new().map_err(err_map!(Unexpected))
}

#[cfg(test)]
mod tests {
    use super::*;

    fn _interval() -> NonRevocedInterval {
        NonRevocedInterval {
            from: None,
            to: Some(123),
        }
    }

    #[test]
    fn test_encode_attribute() {
        assert_eq!(
            encode_credential_attribute("101 Wilson Lane").unwrap(),
            "68086943237164982734333428280784300550565381723532936263016368251445461241953"
        );
        assert_eq!(encode_credential_attribute("87121").unwrap(), "87121");
        assert_eq!(
            encode_credential_attribute("SLC").unwrap(),
            "101327353979588246869873249766058188995681113722618593621043638294296500696424"
        );
        assert_eq!(
            encode_credential_attribute("101 Tela Lane").unwrap(),
            "63690509275174663089934667471948380740244018358024875547775652380902762701972"
        );
        assert_eq!(
            encode_credential_attribute("UT").unwrap(),
            "93856629670657830351991220989031130499313559332549427637940645777813964461231"
        );
        assert_eq!(
            encode_credential_attribute("").unwrap(),
            "102987336249554097029535212322581322789799900648198034993379397001115665086549"
        );
        assert_eq!(
            encode_credential_attribute("None").unwrap(),
            "99769404535520360775991420569103450442789945655240760487761322098828903685777"
        );
        assert_eq!(encode_credential_attribute("0").unwrap(), "0");
        assert_eq!(encode_credential_attribute("1").unwrap(), "1");

        // max i32
        assert_eq!(
            encode_credential_attribute("2147483647").unwrap(),
            "2147483647"
        );
        assert_eq!(
            encode_credential_attribute("2147483648").unwrap(),
            "26221484005389514539852548961319751347124425277437769688639924217837557266135"
        );

        // min i32
        assert_eq!(
            encode_credential_attribute("-2147483648").unwrap(),
            "-2147483648"
        );
        assert_eq!(
            encode_credential_attribute("-2147483649").unwrap(),
            "68956915425095939579909400566452872085353864667122112803508671228696852865689"
        );

        assert_eq!(
            encode_credential_attribute("0.0").unwrap(),
            "62838607218564353630028473473939957328943626306458686867332534889076311281879"
        );
        assert_eq!(
            encode_credential_attribute("\x00").unwrap(),
            "49846369543417741186729467304575255505141344055555831574636310663216789168157"
        );
        assert_eq!(
            encode_credential_attribute("\x01").unwrap(),
            "34356466678672179216206944866734405838331831190171667647615530531663699592602"
        );
        assert_eq!(
            encode_credential_attribute("\x02").unwrap(),
            "99398763056634537812744552006896172984671876672520535998211840060697129507206"
        );
    }

    #[test]
    fn get_non_revoc_interval_for_global() {
        let res = get_non_revoc_interval(&Some(_interval()), &None).unwrap();
        assert_eq!(_interval(), res);
    }

    #[test]
    fn get_non_revoc_interval_for_local() {
        let res = get_non_revoc_interval(&None, &Some(_interval())).unwrap();
        assert_eq!(_interval(), res);
    }

    #[test]
    fn get_non_revoc_interval_for_none() {
        let res = get_non_revoc_interval(&None, &None);
        assert_eq!(None, res);
    }
}
