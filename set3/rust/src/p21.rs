use rust_utils::*;

/*----------------------------------------------------------------------------*/

pub fn main() {
    let mut mt = mersenne::seed(1);
    for i in 0..10 {
        println!("{}: {}", i, mt.next());
    }
}
