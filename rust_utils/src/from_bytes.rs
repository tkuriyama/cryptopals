extern crate base64;
extern crate hex;

pub fn to_hex(input: Vec<u8>) -> String {
    hex::encode(input)
}

pub fn to_b64(input: Vec<u8>) -> String {
    base64::encode(input)
}
