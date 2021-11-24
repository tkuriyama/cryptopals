use rust_utils::*;
use std::fs;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let data = fs::read_to_string("data/p6.txt")
        .expect("Something went wrong reading the file")
        .replace("\n", "");
    let decoded = to_bytes::from_b64(&data);

    let decrypted = decrypt_xor::multi_char_xor(&decoded, 2, 40);
    let plaintext = from_bytes::to_utf8(&decrypted);

    println!("{}", plaintext);
}
