use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let msg = to_bytes::from_utf8("hello world, does this work?");
    let key = 0xff32;

    let cipher = mersenne::apply_ctr(&msg, key);
    let plaintext = mersenne::apply_ctr(&cipher, key);
    println!("Roundtrip plaintext: {}", from_bytes::to_utf8(&plaintext));

    let msg2 = to_bytes::from_utf8("AAAAAAAAAAAAAAasasdn21!");
    let ciphertext = mersenne::apply_ctr(&plaintext, key);
    for key_guess in 1..=65535 {
        let guess = mersenne::apply_ctr(&ciphertext, key_guess);
        if &guess[0..14] == &plaintext[0..14] {
            println!("Key / Guessed Key is {} / {}", key, key_guess);
        }
    }
}
