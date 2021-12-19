use crate::*;
use std::collections::HashMap;

/*----------------------------------------------------------------------------*/
// Block Size == Key Length

pub fn find_key_length(oracle: &impl Fn(&[u8]) -> Vec<u8>) -> u8 {
    let msg = [0].repeat(1);
    let mut prev_len = oracle(&msg).len();
    let mut key_len = 0;
    let mut first_boundary = 0;

    for length in 2..512 {
        let encrypted_len = oracle(&[0].repeat(length)).len();

        if encrypted_len > prev_len && first_boundary > 0 {
            break;
        } else if encrypted_len > prev_len {
            prev_len = encrypted_len;
            first_boundary = key_len;
        }
        key_len += 1;
    }

    key_len - first_boundary
}

/*----------------------------------------------------------------------------*/
// Byte-by-Byte Decryption

pub fn decrypt_oracle(oracle: &impl Fn(&[u8]) -> Vec<u8>, block_size: u8) -> Vec<u8> {
    let block_ct = oracle(&[]).len() / block_size as usize;
    let mut prev_block: Vec<u8> = [0].repeat(block_size as usize).to_vec();
    let mut plaintext: Vec<u8> = Vec::new();

    for i in 0..block_ct {
        let decrypted = decrypt_block(&oracle, i as usize, &prev_block, block_size);
        plaintext.extend(&decrypted);
        prev_block = decrypted.clone();
    }
    plaintext
}

fn decrypt_block(
    oracle: &impl Fn(&[u8]) -> Vec<u8>,
    index: usize,
    prev_block: &[u8],
    block_size: u8,
) -> Vec<u8> {
    let mut new_block = Vec::new();

    for shift in 1..17 {
        let working_block: &Vec<u8> =
            &vector::to_blocks(&oracle(&prev_block[shift..]), block_size as usize)[index];

        let block_map = gen_block_map(&oracle, &prev_block[shift..], block_size, &new_block);
        let byte: u8 = match block_map.get(working_block) {
            Some(b) => *b,
            None => 0,
        };
        new_block = append(&new_block, byte);
    }
    new_block
}

fn gen_block_map(
    oracle: &impl Fn(&[u8]) -> Vec<u8>,
    prev_block: &[u8],
    block_size: u8,
    new_block: &[u8],
) -> HashMap<Vec<u8>, u8> {
    (0..256)
        .map(|i| {
            (
                oracle(&[prev_block, new_block, &[i as u8]].concat())[..block_size as usize]
                    .to_vec(),
                i as u8,
            )
        })
        .into_iter()
        .collect()
}

fn append(v: &[u8], byte: u8) -> Vec<u8> {
    [v, &[byte]].concat()
}
