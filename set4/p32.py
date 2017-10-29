import time
import numpy as np
import requests

URL_SECURE = 'http://127.0.0.1:5000/test'
URL_INSECURE = 'http://127.0.0.1:5000/testinsecure'
PAYLOAD = {'file': 'foo',
           'signature': '274b7c4d98605fcf739a0bf9237551623f415fb8'}
    
# Timing Leak Attack

def time_guess(url, payload, n=10):
    """Return avg response of given request."""
    times = []
    for i in range(n):
        start = time.time()
        r = requests.get(url, params=payload)
        end = time.time()
        times.append((end - start) * 1000)
    return np.mean(times), np.std(times)

def check_valid(url, payload):
    """Return true if url and payload yields status 200."""
    r = requests.get(url, params=payload)
    return True if r.status_code == 200 else False

def gen_params(guess):
    """Return dict of parameters with guess string."""
    return {'file': 'foo', 'signature': guess}

def t_stat(mu1, sigma1, mu2, sigma2, n=10):
    """T-test for significant difference."""
    pooled_sigma = np.sqrt((sigma1**2 + sigma2**2) / 2)
    return (mu2 - mu1) / (pooled_sigma * np.sqrt(2 / n))

def next_guess(url, guess, n=10):
    """Guess next char."""
    base_mu, base_sigma = time_guess(url, gen_params(guess))
    found = []
    for i in range(256):
        c = format(i, 'x')
        test_mu, test_sigma = time_guess(url, gen_params(guess + c), n)
        t = t_stat(base_mu, base_sigma, test_mu, test_sigma, n)
        found.append((c, t))
        
    return max(found, key=lambda x: x[1])
        
def attack(url, n=10):
    """Implement timing attack on given url."""
    guess = ''
    for i in range(20):
        c = next_guess(url, guess, n)
        guess += c[0]
        
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
