use openssl::symm::Cipher;
use rust_utils::*;

/*----------------------------------------------------------------------------*/
// Encode-Decode Roundtrips
#[test]
fn test_ecb_roundtrip() {
    let msg = b"Hello, World! This is a test.";
    let key = b"YELLOW SUBMARINE";

    let encrypted = aes::encrypt_ecb(msg, key, None);
    let decrypted = aes::decrypt_ecb(&encrypted, key, None);

    assert_eq!(decrypted, msg);
}

#[test]
fn test_cbc_roundtrip() {
    let msg = b"Hello, World! This is a test.";
    let key = b"YELLOW SUBMARINE";
    let iv = [0].repeat(16);

    let encrypted = aes::encrypt_cbc(Cipher::aes_128_ecb(), 16, msg, key, &iv);
    let decrypted = aes::decrypt_cbc(Cipher::aes_128_ecb(), 16, &encrypted, key, &iv);

    assert!(decrypted.is_ok());
    assert_eq!(decrypted.unwrap(), msg);
}
