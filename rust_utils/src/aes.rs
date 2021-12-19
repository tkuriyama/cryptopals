use crate::*;
use openssl::symm::{decrypt, encrypt, Cipher, Crypter, Mode};
use std::collections::HashMap;

/*----------------------------------------------------------------------------*/
// ECB, default AES 128

pub fn encrypt_ecb(msg: &[u8], key: &[u8], iv: Option<&[u8]>) -> Vec<u8> {
    encrypt(Cipher::aes_128_ecb(), &key, iv, &msg).unwrap()
}

pub fn decrypt_ecb(msg: &[u8], key: &[u8], iv: Option<&[u8]>) -> Vec<u8> {
    decrypt(Cipher::aes_128_ecb(), &key, iv, &msg).unwrap()
}

/*----------------------------------------------------------------------------*/
// CBC

pub fn encrypt_cbc(
    cipher: Cipher,
    block_size: usize,
    msg: &[u8],
    key: &[u8],
    iv: &[u8],
) -> Vec<u8> {
    let mut crypter = gen_crypter(cipher, Mode::Encrypt, key);

    let blocks = vector::to_blocks(&pkcs7::pad_n(msg, block_size as u8), block_size);
    let mut prev = iv.to_vec().clone();
    let mut encrypted_block = vec![0 as u8; block_size + cipher.block_size()];
    let mut ciphertext: Vec<u8> = Vec::new();

    for block in blocks {
        let _ = crypter
            .update(&vector::xor(&block, &prev), &mut encrypted_block)
            .expect("ECB encrypt failed");
        ciphertext.extend_from_slice(&encrypted_block[..block_size]);
        prev = encrypted_block.clone();
    }

    ciphertext
}

pub fn decrypt_cbc(
    cipher: Cipher,
    block_size: usize,
    msg: &[u8],
    key: &[u8],
    iv: &[u8],
) -> Result<Vec<u8>, &'static str> {
    let mut crypter = gen_crypter(cipher, Mode::Decrypt, key);

    let blocks = vector::to_blocks(msg, block_size);
    let mut prev = iv.to_vec().clone();
    let mut decrypted_block = vec![0 as u8; block_size + cipher.block_size()];
    let mut plaintext: Vec<u8> = Vec::new();

    for block in blocks {
        let _ = crypter
            .update(&block, &mut decrypted_block)
            .expect("ECB decrypt failed");
        plaintext.extend_from_slice(&vector::xor(&decrypted_block, &prev));
        prev = block.clone();
    }

    match pkcs7::strip(&plaintext) {
        Some(stripped) => Ok(stripped),
        None => Err("Invalid padding"),
    }
}

/*----------------------------------------------------------------------------*/
// Crypter

pub fn gen_crypter(cipher: Cipher, mode: Mode, key: &[u8]) -> Crypter {
    let mut crypter = Crypter::new(cipher, mode, key, None).unwrap();
    crypter.pad(false);
    crypter
}

/*----------------------------------------------------------------------------*/
// CBC & ECB Oracle (AES 128)

pub fn encrypt_ecb_or_cbc(v: &[u8]) -> (Vec<u8>, &'static str) {
    let key = random::rand_vec(16);
    let iv = random::rand_vec(16);
    let msg = [
        &random::rand_vec(random::rand_range(5, 11)),
        v,
        &random::rand_vec(random::rand_range(5, 11)),
    ]
    .concat();
    match random::rand_bool() {
        false => (encrypt_ecb(&msg, &key, None), "ECB"),
        true => (
            encrypt_cbc(Cipher::aes_128_ecb(), 16, &msg, &key, &iv),
            "CBC",
        ),
    }
}

pub fn score_ecb_blocks(v: &[u8]) -> u8 {
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

pub fn oracle_ecb_cbc() -> (&'static str, &'static str) {
    let input = [100].repeat(100);
    let (encrypted, mode) = encrypt_ecb_or_cbc(&input);
    if score_ecb_blocks(&encrypted) >= 3 {
        ("ECB", mode)
    } else {
        ("CBC", mode)
    }
}
