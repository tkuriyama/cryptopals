use crate::*;
use openssl::symm::{Cipher, Crypter, Mode};

/*----------------------------------------------------------------------------*/
// CBC

pub fn encrypt_cbc(
    cipher: Cipher,
    block_size: usize,
    msg: &mut Vec<u8>,
    key: &Vec<u8>,
    iv: &Vec<u8>,
) -> Vec<u8> {
    let mut crypter = gen_crypter(cipher, Mode::Encrypt, key);

    let blocks = vector::to_blocks(&pkcs7::pad_n(msg, block_size as u8), block_size);
    let mut prev = iv.clone();
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
    msg: &Vec<u8>,
    key: &Vec<u8>,
    iv: &Vec<u8>,
) -> Result<Vec<u8>, &'static str> {
    let mut crypter = gen_crypter(cipher, Mode::Decrypt, key);

    let blocks = vector::to_blocks(msg, block_size);
    let mut prev = iv.clone();
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
// Crypted

pub fn gen_crypter(cipher: Cipher, mode: Mode, key: &Vec<u8>) -> Crypter {
    let mut crypter = Crypter::new(cipher, mode, key, None).unwrap();
    crypter.pad(false);
    crypter
}
