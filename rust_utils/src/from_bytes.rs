extern crate base64;
extern crate hex;

use std::str;

/*----------------------------------------------------------------------------*/

pub fn to_hex(input: &Vec<u8>) -> String {
    hex::encode(input)
}

pub fn to_b64(input: &Vec<u8>) -> String {
    base64::encode(input)
}

pub fn to_utf8(input: &Vec<u8>) -> String {
    match str::from_utf8(input) {
        Ok(s) => s.to_string(),
        _ => "Could not encode bytes as UTF-8 string".to_string(),
    }
    //   .expect("Encoding to string failed")
    //   .to_string()
}
