extern crate hex;

pub fn from_hex(input: &str) -> Vec<u8> {
    hex::decode(input).expect("Decoding failed")
}
