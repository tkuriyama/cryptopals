use openssl::*;
use rust_utils::*;
use std::fs;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let data = fs::read_to_string("data/p7.txt")
        .expect("file read failed")
        .replace("\n", "");
    let decoded = to_bytes::from_b64(&data);
    let key = b"YELLOW SUBMARINE";
    let decrypted = symm::decrypt(symm::Cipher::aes_128_ecb(), key, None, &decoded);
    match decrypted {
        Ok(vec) => println!("{}", from_bytes::to_utf8(&vec)),
        _ => println!("Failed to decrypt."),
    }
}
