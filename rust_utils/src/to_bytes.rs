extern crate base64;
extern crate hex;

/*----------------------------------------------------------------------------*/

pub fn from_hex(input: &str) -> Vec<u8> {
    hex::decode(input).expect("Decoding from hex failed")
}

pub fn from_b64(input: &str) -> Vec<u8> {
    base64::decode(input).expect("Decoding from base64 failed")
}

pub fn from_utf8(input: &str) -> Vec<u8> {
    input.as_bytes().to_vec()
}

pub fn from_u64(little_endian: bool, n: u64) -> Vec<u8> {
    match little_endian {
        true => n.to_le_bytes().to_vec(),
        false => n.to_be_bytes().to_vec(),
    }
}
