use openssl::symm::Cipher;
use rust_utils::*;
use std::fs;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let data = fs::read_to_string("data/p10.txt")
        .expect("file read failed")
        .replace("\n", "");

    let decoded = to_bytes::from_b64(&data);
    let key = to_bytes::from_utf8("YELLOW SUBMARINE");
    let iv = vec![0].repeat(16);
    let decrypted = aes::decrypt_cbc(Cipher::aes_128_ecb(), 16, &decoded, &key, &iv);
    match decrypted {
        Ok(msg) => println!("{:?}", from_bytes::to_utf8(&msg)),
        Err(err) => println!("{}", err),
    }
}
