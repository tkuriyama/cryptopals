quse rust_utils::*;

pub fn main() {
    let decoded = to_bytes::from_hex("49276d206b696c6c696e6720796f757220627261696e206c696b65206120706f69736f6e6f7573206d757368726f6f6d");

    let encoded = from_bytes::to_b64(decoded);

    let matches = encoded == "SSdtIGtpbGxpbmcgeW91ciBicmFpbiBsaWtlIGEgcG9pc29ub3VzIG11c2hyb29t";

    println!("{}\nEncoded string matches: {}", encoded, matches);
}
