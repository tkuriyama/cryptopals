import time
import numpy as np
import requests

URL_SECURE = 'http://127.0.0.1:5000/test'
URL_INSECURE = 'http://127.0.0.1:5000/testinsecure'
PAYLOAD = {'file': 'foo',
           'signature': '274b7c4d98605fcf739a0bf9237551623f415fb8'}

# Timing Leak Attack

def time_guess(url, payload):
    """Return avg response of given request."""
    times = []
    for i in range(5):
        start = time.time()
        r = requests.get(url, params=payload)
        end = time.time()
        times.append((end - start) * 1000)
    return np.mean(times)

def check_valid(url, payload):
    """Return true if url and payload yields status 200."""
    r = requests.get(url, params=payload)
    return True if r.status_code == 200 else False

def gen_params(guess):
    """Return dict of parameters with guess string."""
    return {'file': 'foo', 'signature': guess}

def next_guess(url, guess):
    """Guess next char."""
    baseline = time_guess(url, gen_params(guess))
    found = False
    for i in range(256):
        c = format(i, 'x')
        test = time_guess(url, gen_params(guess + c))
        if (test - baseline) > 40:
            found = True
            break
    return c, found
        
def attack(url):
    """Implement timing attack on given url."""
    guess = ''
    found = True
    while found:
        c, found = next_guess(url, guess)
        guess += c if found else ''
        
    valid = check_valid(url, gen_params(guess))
    return valid, guess
    
# Baseline

def print_perf(start, end):
    """Print diff as ms between start and end times (floats in secs)."""
    diff = (end - start) * 1000
    print('{:.2f} milliseconds elapsed'.format(diff))
    return diff

def test_baseline(url, msg):
    """Test secure entrypoint."""
    start = time.time()
    r = requests.get(url, params=PAYLOAD)
    end = time.time()
    print(msg)
    print_perf(start, end)
    assert r.status_code == 200

# Main
    
def main():
    """Main."""
    test_baseline(URL_SECURE, 'secure')
    test_baseline(URL_INSECURE, 'insecure')
    attack(URL_INSECURE)
    
if __name__ == '__main__':
    main()
