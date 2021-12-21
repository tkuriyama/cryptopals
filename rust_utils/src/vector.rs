pub fn xor(v1: &[u8], v2: &[u8]) -> Vec<u8> {
    v1.iter().zip(v2.iter()).map(|(&x1, &x2)| x1 ^ x2).collect()
}

pub fn transpose<T>(v: Vec<Vec<T>>) -> Vec<Vec<T>> {
    assert!(!v.is_empty());
    let len = v[0].len();
    let mut iters: Vec<_> = v.into_iter().map(|n| n.into_iter()).collect();
    (0..len)
        .map(|_| {
            iters
                .iter_mut()
                .map(|n| n.next().unwrap())
                .collect::<Vec<T>>()
        })
        .collect()
}

/*----------------------------------------------------------------------------*/

pub fn to_blocks(v: &[u8], size: usize) -> Vec<Vec<u8>> {
    let blocks = v.chunks_exact(size).map(|block| block.to_vec()).collect();
    blocks
}

/*----------------------------------------------------------------------------*/

pub fn merge(v1: &[u8], v2: &[u8]) -> Vec<u8> {
    [v1, v2].concat()
}
