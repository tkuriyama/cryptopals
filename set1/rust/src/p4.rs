use rust_utils::*;
use std::fs;
/*----------------------------------------------------------------------------*/

pub fn main() {
    let data = fs::read_to_string("data/p4.txt").expect("Something went wrong reading the file");

    let mut scored = Vec::new();
    for s in data.split("\n") {
        let v = to_bytes::from_hex(s);
        scored.push(decrypt_xor::single_char_xor(&v));
    }
    scored.sort_by(|a, b| a.1.partial_cmp(&b.1).unwrap());

    for (c, score, msg) in &scored[..5] {
        println!("Char {} Score {} {}", c, score, from_bytes::to_utf8(&msg));
    }
}
