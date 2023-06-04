use rust_utils::*;
use std::fs;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let lines: Vec<Vec<u8>> = fs::read_to_string("data/p19.txt")
        .expect("file read failed")
        .lines()
        .map(to_bytes::from_b64)
        .collect();

    let min_len = lines.iter().map(|x| x.len()).min().unwrap();
    println!("Min length is {}", min_len);

    let code: Vec<u8> = lines.iter().flat_map(|x| x[..min_len].to_vec()).collect();
    let plaintext = decrypt_xor::multi_char_xor(&code, min_len as u8, min_len as u8);
    for line in vector::to_blocks(&plaintext, min_len) {
        println!("{}", from_bytes::to_utf8(&line));
    }

}
