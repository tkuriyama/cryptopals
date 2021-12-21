use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let oracle = encrypt_ecb_oracle();

    let block_size = decrypt_ecb::find_key_length(&oracle);
    println!("Key length: {}", block_size);

    let (prefix, start_index) = decrypt_ecb::inspect_oracle(&oracle, block_size);

    let ecb_mode = if aes::score_ecb_blocks(&oracle(&[0].repeat(100))) >= 3 {
        true
    } else {
        false
    };
    println!("ECB mode: {}", ecb_mode);

    let decrypted = decrypt_ecb::decrypt_oracle(&oracle, block_size, &prefix, start_index as usize);
    println!(
        "\nPlaintext:\n{}, {:?}",
        from_bytes::to_utf8(&decrypted),
        decrypted
    );
}

/*----------------------------------------------------------------------------*/

fn encrypt_ecb_oracle() -> impl Fn(&[u8]) -> Vec<u8> {
    let key = random::rand_vec(16);
    let prepend = random::rand_vec(random::rand_range(2, 40));
    let append =
        to_bytes::from_b64("Um9sbGluJyBpbiBteSA1LjAKV2l0aCBteSByYWctdG9wIGRvd24gc28gbXkgaGFpciBjYW4gYmxvdwpUaGUgZ2lybGllcyBvbiBzdGFuZGJ5IHdhdmluZyBqdXN0IHRvIHNheSBoaQpEaWQgeW91IHN0b3A/IE5vLCBJIGp1c3QgZHJvdmUgYnkK");

    println!(
        "[Secret: This oracle prepends {} random bytes]",
        prepend.len()
    );
    let encrypter = move |v: &[u8]| -> Vec<u8> {
        let msg = [&prepend, v, &append].concat();
        aes::encrypt_ecb(&msg, &key, None)
    };

    encrypter
}
