pub fn problem_separator(s: &str) {
    let n: usize = (80 - s.len()) / 2;
    let line = "-".repeat(n);
    println!("\n{} {} {}\n", line, s, line);
}
