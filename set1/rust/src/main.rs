use rust_utils::*;
mod p1;
mod p2;
mod p3;

/*----------------------------------------------------------------------------*/

fn main() {
    print::problem_separator("P1");
    p1::main();
    print::problem_separator("P2");
    p2::main();
    print::problem_separator("P3");
    p3::main();
}
