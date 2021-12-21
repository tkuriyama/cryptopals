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

pub fn decrypt_oracle(
    oracle: &impl Fn(&[u8]) -> Vec<u8>,
    block_size: u8,
    prefix: &[u8],
    start_index: usize,
) -> Vec<u8> {
    let block_ct = oracle(prefix).len() / block_size as usize;
    let mut prev_block: Vec<u8> = [0].repeat(block_size as usize).to_vec();
    let mut plaintext: Vec<u8> = Vec::new();

    for i in start_index..block_ct {
        let decrypted = decrypt_block(
            &oracle,
            i as usize,
            &prev_block,
            block_size,
            &prefix,
            start_index,
        );
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
    prefix: &[u8],
    start_index: usize,
) -> Vec<u8> {
    let mut new_block = Vec::new();

    for shift in 1..17 {
        let working_block: &Vec<u8> = &vector::to_blocks(
            &oracle(&vector::merge(prefix, &prev_block[shift..])),
            block_size as usize,
        )[index];

        let block_map = gen_block_map(
            &oracle,
            &prev_block[shift..],
            block_size,
            &new_block,
            prefix,
            start_index,
        );
        let byte: u8 = match block_map.get(working_block) {
            Some(b) => *b,
            None => 0,
        };
        new_block = vector::merge(&new_block, &[byte as u8]);
    }
    new_block
}

fn gen_block_map(
    oracle: &impl Fn(&[u8]) -> Vec<u8>,
    prev_block: &[u8],
    block_size: u8,
    new_block: &[u8],
    prefix: &[u8],
    start_index: usize,
) -> HashMap<Vec<u8>, u8> {
    (0..256)
        .map(|i| {
            (
                vector::to_blocks(
                    &oracle(&[prefix, prev_block, new_block, &[i as u8]].concat()),
                    block_size as usize,
                )[start_index]
                    .to_vec(),
                i as u8,
            )
        })
        .into_iter()
        .collect()
}

/*----------------------------------------------------------------------------*/
// Inspect Oracle for random prefix and find a prefix and the index required to
// compensate for the prefix

pub fn inspect_oracle(oracle: &impl Fn(&[u8]) -> Vec<u8>, block_size: u8) -> (Vec<u8>, u32) {
    let block_index = find_msg_block(oracle, block_size);
    println!(
        "Oracle random prefix block ends at block index: {}",
        block_index
    );
    let prefix_len = find_prefix_len(oracle, block_size, block_index);
    println!(
        "Bytes required to fully pad the random prefix: {}",
        prefix_len
    );

    ([0].repeat(prefix_len as usize).to_vec(), block_index)
}

pub fn find_msg_block(oracle: &impl Fn(&[u8]) -> Vec<u8>, block_size: u8) -> u32 {
    let b1 = vector::to_blocks(&oracle(&Vec::new()), block_size as usize);
    let b2 = vector::to_blocks(&oracle(&vec![0]), block_size as usize);

    let mut block_index = 0;
    for i in 0..b2.len() {
        if b1[i] != b2[i] {
            block_index = i as u32;
            break;
        }
    }
    block_index + 1
}

pub fn find_prefix_len(oracle: &impl Fn(&[u8]) -> Vec<u8>, block_size: u8, block_index: u32) -> u8 {
    let repeated = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15].repeat(2);
    let mut prefix_len: u8 = 0;

    for i in 0..16 {
        let m = vector::merge(&[0].repeat(i), &repeated);
        let blocks = vector::to_blocks(&oracle(&m), block_size as usize);
        if blocks[block_index as usize] == blocks[block_index as usize + 1] {
            break;
        }
        prefix_len += 1;
    }
    prefix_len
}
