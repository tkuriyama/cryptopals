use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    for _ in 0..10 {
        let (guess, answer) = aes::oracle_ecb_cbc();
        if guess != answer {
            println!(">>> Oracle error: {} vs actually {}", guess, answer);
        } else {
            println!("Oracle correct: {}, {}.", guess, answer);
        }
    }
}
