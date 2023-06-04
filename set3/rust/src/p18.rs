use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let key = to_bytes::from_utf8("YELLOW SUBMARINE");
    let code = to_bytes::from_b64("L77na/nrFsKvynd6HzOoG7GHTLXsTVu9qvY/2syLXzhPweyyMTJULu/6/kXX0KSvoOLSFQ==");
    let plaintext = aes::apply_ctr(&code, &key, 0, None);
    println!("plaintext {:?}", plaintext);
    println!("Plaintext {}", from_bytes::to_utf8_by_block(&plaintext, 16, " ? "));

    let msg = "The lazy brown fox jumps over the white dog.";
    ctr_roundtrip(&to_bytes::from_utf8(msg), &key);
}

fn ctr_roundtrip(msg: &Vec<u8>, key: &Vec<u8>) {
    println!("\n> Test Roundtrip");
    println!("Original: {}", from_bytes::to_utf8(msg));
    let code = aes::apply_ctr(&msg, key, 0, None);
    println!("Encrypted: {}", from_bytes::to_utf8(&code));
    let plaintext = aes::apply_ctr(&code, key, 0, None);
    println!("Recovered: {}", from_bytes::to_utf8(&plaintext));
}
