pub fn xor(v1: &Vec<u8>, v2: &Vec<u8>) -> Vec<u8> {
    v1.iter().zip(v2.iter()).map(|(&x1, &x2)| x1 ^ x2).collect()
}
