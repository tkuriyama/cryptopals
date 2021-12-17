use openssl::symm::{Cipher, Mode};
use rust_utils::*;

/*----------------------------------------------------------------------------*/
// Encode-Decode Roundtrips

#[test]
fn test_aes_roundtrip() {
    let mut msg = to_bytes::from_utf8("Hello, World! This is a test.");
    let key = to_bytes::from_utf8("YELLOW SUBMARINE");
    let iv = vec![0].repeat(16);

    let encrypted = aes::encrypt_cbc(Cipher::aes_128_ecb(), 16, &mut msg, &key, &iv);
    println!("Encrypted: {:?}", encrypted);

    let decrypted = aes::decrypt_cbc(Cipher::aes_128_ecb(), 16, &encrypted, &key, &iv);

    assert!(decrypted.is_ok());
}
