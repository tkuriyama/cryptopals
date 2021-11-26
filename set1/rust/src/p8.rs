use rust_utils::*;
use std::collections::HashMap;
use std::fs;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let data = fs::read_to_string("data/p8.txt").expect("file read failed");

    let mut scored = Vec::new();
    for s in data.split("\n") {
        let v = to_bytes::from_hex(s);
        scored.push((score_ecb_blocks(&v), v));
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

fn score_ecb_blocks(v: &Vec<u8>) -> u8 {
    let mut count_map = HashMap::new();

    let blocks = vector::to_blocks(v, 16);
    for block in blocks {
        let count = count_map.entry(block).or_insert(0);
        *count += 1;
    }

    let (_, max_score) = count_map
        .into_iter()
        .max_by(|(_, v1), (_, v2)| v1.cmp(v2))
        .unwrap_or((Vec::new(), 0));
    max_score
}
