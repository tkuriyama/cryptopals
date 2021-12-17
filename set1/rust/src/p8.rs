use rust_utils::*;
use std::fs;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let data = fs::read_to_string("data/p8.txt").expect("file read failed");

    let mut scored = Vec::new();
    for s in data.split("\n") {
        let v = to_bytes::from_hex(s);
        scored.push((aes::score_ecb_blocks(&v), v));
    }

    scored.sort_by(|a, b| b.0.cmp(&a.0));
    for (s, encrypted) in &scored[..3] {
        println!(
            "Repeating blocks {}: {}...",
            s,
            &from_bytes::to_hex(&encrypted)[..20]
        );
    }
}
