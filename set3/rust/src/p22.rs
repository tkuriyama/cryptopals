use rand::Rng;
use rust_utils::*;
use std::thread;
use std::time::{Duration, SystemTime, UNIX_EPOCH};

/*----------------------------------------------------------------------------*/

pub fn main() {
    let seed = now();
    println!("Seed: {}", seed);
    let mut mt = mersenne::seed(seed);

    let mut rng = rand::thread_rng();
    let random_seconds = rng.gen_range(1..=5);
    let duration = Duration::new(random_seconds, 0);
    thread::sleep(duration);

    let i = mt.next();
    println!("Slept for random seconds... MT output: {}", i);
    find_seed(i);
}

fn now() -> u32 {
    let start = SystemTime::now();
    let since_the_epoch = start
        .duration_since(UNIX_EPOCH)
        .expect("Time went backwards");
    since_the_epoch.as_secs() as u32
}

/*----------------------------------------------------------------------------*/

fn find_seed(n: u32) {
    let t = now();
    for offset in 1..50 {
        let mut mt = mersenne::seed(t - (offset as u32));
        if n == mt.next() {
            println!("Recovered seed is: {}", t - offset);
        }
    }
}
