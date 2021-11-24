use crate::*;

/*----------------------------------------------------------------------------*/
// Decrypt Single-character XOR

pub fn single_char_xor(v: &Vec<u8>) -> (u8, f32, Vec<u8>) {
    single_char_xors(&v).swap_remove(0)
}

pub fn single_char_xors(v: &Vec<u8>) -> Vec<(u8, f32, Vec<u8>)> {
    let mut scored: Vec<(u8, f32, Vec<u8>)> = reference::ascii_chars()
        .into_iter()
        .map(|c| find_score(c, v))
        .collect();

    scored.sort_by(|a, b| a.1.partial_cmp(&b.1).unwrap());

    scored
}

fn find_score(c: u8, msg: &Vec<u8>) -> (u8, f32, Vec<u8>) {
    let v = vec![c].repeat(msg.len());
    let xored = vector::xor(&v, msg);

    (c, metrics::score_alphabetic(&xored), xored)
}

/*----------------------------------------------------------------------------*/
// Decrypt Multi-character XOR (Vignere Cipher)

pub fn multi_char_xor(v: &Vec<u8>, key_low: u8, key_high: u8) -> Vec<u8> {
    let key_size = find_key_size(v, key_low, key_high);
    let transposed: Vec<Vec<u8>> = vector::transpose(
        v.chunks_exact(key_size as usize)
            .map(|row| row.to_vec())
            .collect(),
    );

    let key: Vec<u8> = transposed
        .iter()
        .map(|row| single_char_xor(row))
        .map(|(c, _, _)| c)
        .collect();
    let key_ = key.repeat(v.len() / (key_size as usize) + 1);

    vector::xor(v, &key_)
}

fn find_key_size(v: &Vec<u8>, low: u8, high: u8) -> u8 {
    let (_, key_size) = find_key_sizes(v, low, high).swap_remove(0);
    key_size
}

fn find_key_sizes(v: &Vec<u8>, low: u8, high: u8) -> Vec<(f32, u8)> {
    let mut pairs = Vec::new();
    for guess in low..=high {
        if v.len() > (guess * 4).into() {
            pairs.push((score_key(guess, v), guess));
        }
    }
    pairs.sort_by(|a, b| a.0.partial_cmp(&b.0).unwrap());
    pairs
}

// score key size as average Hamming distance normalized by key size
fn score_key(size: u8, v: &Vec<u8>) -> f32 {
    let n = size as usize;
    let m1 = &v[0..n].to_vec();
    let m2 = &v[n..2 * n].to_vec();
    let m3 = &v[2 * n..3 * n].to_vec();
    let m4 = &v[3 * n..4 * n].to_vec();

    let sum = metrics::hamming_distance(m1, m2)
        + metrics::hamming_distance(m1, m3)
        + metrics::hamming_distance(m1, m4)
        + metrics::hamming_distance(m2, m3)
        + metrics::hamming_distance(m2, m4)
        + metrics::hamming_distance(m3, m4);

    (sum as f32) / (6.0 * (size as f32))
}
