from __future__ import division
from itertools import combinations
import utils

def get_blocks(code, size, n=4):
    """Get first n blocks of given size (int) from code."""
    assert size * n <= len(code)
    return [code[i * size: (i + 1) * size] for i in xrange(n)]

def edit_scores(blocks):
    """Find average edit score of given blocks (of the same size).
    First generate all combo pairs from given list of blocks. Then find the
    avg edit dist, where each pair's edit dist is normalized by block size.
    Returns tuple of score and block size.
    """
    size = len(blocks[0])
    pairs = list(combinations(blocks, 2))
    total = sum(utils.edit_dist(fst, snd) / size for fst, snd in pairs)
    score = total / len(pairs)
    return score, size

def find_keysizes(code, key_min, key_max, n=4):
    """Find keysize in range (min, max) of the given code using n blocks."""
    blocks_arr = [get_blocks(code, size, n)
                  for size in xrange(key_min, key_max)]
    return [edit_scores(blocks) for blocks in blocks_arr]

def guess_keys(code, key_min, key_max, num_blocks, top):
    """Guess key to decrypt code, return top n.
    Args
        code: bytearray of code to decrypt
        key_min: int, min length of key to try
        key_max: int, max lenght of key to try
        num_blocks: int, number of blocks to use in analysis
        top: int, number of top guesses to return
    Returns
        List of key guesses as plaintext strings.
    """
    keysizes = find_keysizes(code, key_min, key_max, num_blocks)
    top_keys = sorted(keysizes, key=lambda x: x[0])[:top]
    key_guesses = []
    for _, keysize in top_keys:
        blocks = get_blocks(code, keysize, int(len(code) / keysize))
        transpose = zip(*blocks)
        key_guess = ''.join([utils.decrypt_single_xor(group)[1]
                             for group in transpose])
        key_guesses.append(key_guess)
    return key_guesses

def decrypt_vigenere(code, key_min, key_max, num_blocks):
    """Decrypt given bytearray encryped using Vignere cipher.
    Args
        code: bytearray of code to decrypt
        key_min: int, min length of key to try
        key_max: int, max lenght of key to try
        num_blocks: int, number of blocks to use in analysis
    Returns
        (score, key, plaintext guess)
    """
    key_guesses = guess_keys(code, key_min, key_max, num_blocks, 3)
    guesses = [(k, ''.join([chr(b) for b in utils.xor(code, bytearray(k))]))
               for k in key_guesses]

    scores = utils.score_guesses(guesses)
    return min(scores, key=lambda x: x[0])

# Main

def main():
    """Main."""
    with open('p6_problem.txt', 'r') as f:
        data = f.read()
    code = bytearray(data.decode('base64'))

    plaintext = decrypt_vigenere(code, 2, 41, 4)
    print plaintext
    print 'p6 ok'

if __name__ == '__main__':
    main()
