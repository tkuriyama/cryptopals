use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let oracle = encrypt_ecb_oracle();

    let block_size = decrypt_ecb::find_key_length(&oracle);
    println!("Key length: {}", block_size);

    let ecb_mode = if aes::score_ecb_blocks(&oracle(&[0].repeat(100))) >= 3 {
        true
    } else {
        false
    };
    println!("ECB mode: {}", ecb_mode);

    let decrypted = decrypt_ecb::decrypt_oracle(&oracle, block_size, &Vec::new(), 0);
    println!(
        "\nPlaintext:\n{}, {:?}",
        from_bytes::to_utf8(&decrypted),
        decrypted
    );
}

/*----------------------------------------------------------------------------*/

fn encrypt_ecb_oracle() -> impl Fn(&[u8]) -> Vec<u8> {
    let key: Vec<u8> = random::rand_vec(16);
    let append: Vec<u8> =
        to_bytes::from_b64("Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkgaGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBqdXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUgYnkK");

    let encrypter = move |v: &[u8]| -> Vec<u8> {
        let msg = [v, &append].concat();
        aes::encrypt_ecb(&msg, &key, None)
    };

    encrypter
}
