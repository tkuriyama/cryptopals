use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let m1 = "1b37373331363f78151b7f2b783431333d78397828372d363c78373e783a393b3736";
    let decoded = to_bytes::from_hex(m1);
    let scored = decrypt_xor::single_char_xors(&decoded);

    for (c, score, msg) in &scored[..5] {
        println!("Char {} Score {} {}", c, score, from_bytes::to_utf8(&msg));
    }
}
