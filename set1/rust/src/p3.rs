use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let m1 = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";
    let decoded = to_bytes::from_hex(m1);

    let mut scored: Vec<(f32, Vec<u8>)> = reference::ascii_chars()
        .into_iter()
        .map(|c| find_score(c, &decoded))
        .collect();

    scored.sort_by(|a, b| a.0.partial_cmp(&b.0).unwrap());

    for (score, msg) in &scored[..5] {
        println!("Score {} {}", score, from_bytes::to_utf8(&msg));
    }
}

/*----------------------------------------------------------------------------*/

fn find_score(c: u8, msg: &Vec<u8>) -> (f32, Vec<u8>) {
    let v = vec![c].repeat(msg.len());
    let xored = vector::xor(&v, msg);

    (metrics::score_alphabetic(&xored), xored)
}
