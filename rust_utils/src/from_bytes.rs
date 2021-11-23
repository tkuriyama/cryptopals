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
    str::from_utf8(input)
        .expect("Encoding to string failed")
        .to_string()
}
