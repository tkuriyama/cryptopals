 use openssl::symm::Cipher;
use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let xs = vec!["MDAwMDAwTm93IHRoYXQgdGhlIHBhcnR5IGlzIGp1bXBpbmc=",
                  "MDAwMDAxV2l0aCB0aGUgYmFzcyBraWNrZWQgaW4gYW5kIHRoZSBWZWdhJ3MgYXJlIHB1bXBpbic=",
                  "MDAwMDAyUXVpY2sgdG8gdGhlIHBvaW50LCB0byB0aGUgcG9pbnQsIG5vIGZha2luZw==",
                  "MDAwMDAzQ29va2luZyBNQydzIGxpa2UgYSBwb3VuZCBvZiBiYWNvbg==",
                  "MDAwMDA0QnVybmluZyAnZW0sIGlmIHlvdSBhaW4ndCBxdWljayBhbmQgbmltYmxl",
                  "MDAwMDA1SSBnbyBjcmF6eSB3aGVuIEkgaGVhciBhIGN5bWJhbA==",
                  "MDAwMDA2QW5kIGEgaGlnaCBoYXQgd2l0aCBhIHNvdXBlZCB1cCB0ZW1wbw==",
                  "MDAwMDA3SSdtIG9uIGEgcm9sbCwgaXQncyB0aW1lIHRvIGdvIHNvbG8=",
                  "MDAwMDA4b2xsaW4nIGluIG15IGZpdmUgcG9pbnQgb2g=",
                  "MDAwMDA5aXRoIG15IHJhZy10b3AgZG93biBzbyBteSBoYWlyIGNhbiBibG93",];
    let vs: Vec<Vec<u8>> = xs.iter().map(|&s| to_bytes::from_b64(s)).collect();

    let key = random::rand_vec(16);
    let (iv, code) = initialize(&vs, &key);
    // let oracle = Oracle::new(key);
    let oracle = move |iv: &Vec<u8>, code: &Vec<u8>| -> bool {
        padding_oracle(&key, iv, code)
    };

    println!("Test valid padding: {} ", &oracle(&iv, &code));

    let decrypted = padding_oracle_attack(&oracle, &code, &iv);
    match pkcs7::strip(&decrypted) {
        Some(bytes) => {
            println!("Padding oracle attack, the original msg is: {}",
                     from_bytes::to_utf8(&bytes));
        }
        None => {
            println!("Decrypted bytes had invalid padding: {:?}", decrypted);
        }
    }
}

fn initialize(xs: &Vec<Vec<u8>>, key: &Vec<u8>) -> (Vec<u8>, Vec<u8>) {
    let iv = random::rand_vec(16);
    let original = &xs[random::rand_range(0, 10) as usize];
    let padded = pkcs7::pad_n(original, 16);
    println!("Plaintext: {:?}", vector::to_blocks(&padded, 16));
    println!("Plaintext: {}", from_bytes::to_utf8(original));
    (iv.clone(), aes::encrypt_cbc(Cipher::aes_128_ecb(), 16, &padded, key, &iv))
}

fn padding_oracle(key: &Vec<u8>, iv: &Vec<u8>, code: &Vec<u8>) -> bool {
    let plaintext = aes::decrypt_cbc_nostrip(Cipher::aes_128_ecb(), 16, code, key, iv);
    pkcs7::valid_padding(&plaintext)
}


/*----------------------------------------------------------------------------*/
// Attack

type Oracle = dyn Fn(&Vec<u8>, &Vec<u8>) -> bool;

fn padding_oracle_attack(oracle: &Oracle, code: &Vec<u8>, iv: &Vec<u8>) -> Vec<u8> {
    let blocks = &vector::to_blocks(code, 16);
    let result: Vec<Vec<u8>> = blocks.iter().map(|b| decrypt_block(oracle, b)).collect();
    let xor = vector::merge(iv, code);
    vector::xor(&result.concat(), &xor)
}

fn decrypt_block(oracle: &Oracle, block: &Vec<u8>) -> Vec<u8> {
    let mut found = vec![];
    for i in 1..=16 {
        let iv = vector::xor(&found, &vec![i; found.len()]);
        let byte = decrypt_byte(oracle, block, &iv);
        found.insert(0, byte ^ i);
    };
    found
}

fn decrypt_byte(oracle: &Oracle, block: &Vec<u8>, found: &Vec<u8>,) -> u8 {
    let i = found.len();
    let mut iv = [vec![0; 16 - i], found.to_vec()].concat();
    for guess in 0..=255 {
        iv[15 - i] = guess;
        if oracle(&iv, block) == true {
            return guess;
        }
    }
    0
}

