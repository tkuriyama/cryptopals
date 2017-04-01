import utils

def main():
    """Main."""
    with open('p4_problem.txt', 'r') as f:
        data = f.read()
        lines = [line.strip() for line in data.split('\n') if line]

    line_scores = [utils.decrypt_single_xor(bytearray.fromhex(line))
                   for line in lines]
    print 'p4', min(line_scores, key=lambda x: x[1])

if __name__ == '__main__':
    main()
