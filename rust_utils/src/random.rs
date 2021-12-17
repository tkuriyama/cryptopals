use rand::*;

/*----------------------------------------------------------------------------*/

pub fn rand_vec(length: u32) -> Vec<u8> {
    let v: Vec<u8> = (0..length)
        .map(|_| rand::thread_rng().gen_range(0..256) as u8)
        .collect();
    v
}

pub fn rand_bool() -> bool {
    match rand::thread_rng().gen_range(0..2) {
        0 => false,
        _ => true,
    }
}

pub fn rand_range(min: u32, max: u32) -> u32 {
    rand::thread_rng().gen_range(min..max)
}
