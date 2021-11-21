use rust_utils::*;

pub fn main() {
    let m1 = to_bytes::from_hex("1c0111001f010100061a024b53535009181c");
    let m2 = to_bytes::from_hex("686974207468652062756c6c277320657965");
    let xored = from_bytes::to_hex(vector::xor(m1, m2));
    let matches = xored == "746865206b696420646f6e277420706c6179";

    println!("{}\nEncoded string matches: {}", xored, matches);
}
