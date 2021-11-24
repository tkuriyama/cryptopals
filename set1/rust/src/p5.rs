use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let msg = "Burning 'em, if you ain't quick and nimble\nI go crazy when I hear a cymbal";
    let key = "ICE";
    let target = "0b3637272a2b2e63622c2e69692a23693a2a3c6324202d623d63343c2a26226324272765272a282b2f20430a652e2c652a3124333a653e2b2027630c692b20283165286326302e27282f";

    let n = msg.len() / 3 + 1;
    let key_ = key.repeat(n);
    let encrypted = from_bytes::to_hex(&vector::xor(
        &to_bytes::from_utf8(msg),
        &to_bytes::from_utf8(&key_),
    ));

    println!("{}\nOutput matches: {}", encrypted, encrypted == target);
}
