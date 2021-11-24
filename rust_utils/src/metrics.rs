extern crate bit_vec;
extern crate lazy_static;

use bit_vec::BitVec;
use lazy_static::lazy_static;
use std::collections::HashMap;

/*----------------------------------------------------------------------------*/

lazy_static! {
    static ref CHAR_FREQ: HashMap<u8, f32> = {
        HashMap::from([
            (b'a', 0.0651738),
            (b'b', 0.0124248),
            (b'c', 0.0217339),
            (b'd', 0.0349835),
            (b'e', 0.1041442),
            (b'f', 0.0197881),
            (b'g', 0.0158610),
            (b'h', 0.0492888),
            (b'i', 0.0558094),
            (b'j', 0.0009033),
            (b'k', 0.0050529),
            (b'l', 0.0331490),
            (b'm', 0.0202124),
            (b'n', 0.0564513),
            (b'o', 0.0596302),
            (b'p', 0.0137645),
            (b'q', 0.0008606),
            (b'r', 0.0497563),
            (b's', 0.0515760),
            (b't', 0.0729357),
            (b'u', 0.0225134),
            (b'v', 0.0082903),
            (b'w', 0.0171272),
            (b'x', 0.0013692),
            (b'y', 0.0145984),
            (b'z', 0.0007836),
            (b' ', 0.1918182),
        ])
    };
}

/*----------------------------------------------------------------------------*/
// Alphabet-Based Scoring

/// Score for likelihood of being valid English plaintext.
/// Uses a chi-squared score based on English letter frequencies.
/// Scores are always positive; lower is better, 0.0 is best.

pub fn score_alphabetic(v: &Vec<u8>) -> f32 {
    let mut char_freq: HashMap<u8, f32> = HashMap::new();
    let total_count = v.len() as f32;

    for c in v.iter() {
        let count = char_freq.entry(c.to_ascii_lowercase()).or_insert(0.0);
        *count += 1.00;
    }

    // modified chi-squared
    let mut score: f32 = 0.0;

    for (c, count) in char_freq.iter() {
        score += match CHAR_FREQ.get(c) {
            Some(ref_freq) => ((count / total_count) - ref_freq).powi(2) / ref_freq,
            None => {
                if ignore_char(c) {
                    0.0
                } else {
                    1.0
                }
            }
        }
    }

    score
}

fn ignore_char(c: &u8) -> bool {
    if *c == b'.' || *c == b',' || *c == b'!' || *c == b'?' {
        true
    } else {
        false
    }
}

/*----------------------------------------------------------------------------*/
// Hamming Distance

/// Hamming distance == bitwise edit / xor distance

pub fn hamming_distance(v1: &Vec<u8>, v2: &Vec<u8>) -> u32 {
    let mut bv1 = BitVec::from_bytes(v1);
    let bv2 = BitVec::from_bytes(v2);

    bv1.xor(&bv2);
    bv1.iter().filter(|&x| x).count() as u32
}

/*----------------------------------------------------------------------------*/

#[cfg(test)]
mod tests {

    use super::*;
    use crate::*;

    #[test]
    fn test_score_alphabet() {
        let v0 = vec![b'e'];
        let v1 = vec![b'e', 0u8];
        let v2 = vec![b' '];
        let v3 = vec![b'e', b',', b'.'];
        let v4 = vec![b'e', b'!', b'?'];

        assert!(score_alphabetic(&v0) > score_alphabetic(&v1));
        assert!(score_alphabetic(&v0) > score_alphabetic(&v2));
        assert!(float::f32_eq(
            score_alphabetic(&v3),
            score_alphabetic(&v4),
            0.0001
        ));
    }

    #[test]
    fn test_hamming() {
        let v1 = to_bytes::from_utf8("this is a test");
        let v2 = to_bytes::from_utf8("wokka wokka!!!");
        assert_eq!(hamming_distance(&v1, &v2), 37);
    }
}
