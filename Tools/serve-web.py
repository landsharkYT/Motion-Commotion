#!/usr/bin/env python3
"""Serve Unity Web output locally with WebAssembly and cross-origin headers."""
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import argparse


class UnityHandler(SimpleHTTPRequestHandler):
    extensions_map = {**SimpleHTTPRequestHandler.extensions_map, ".wasm": "application/wasm"}

    def end_headers(self):
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        super().end_headers()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", type=int, default=8765)
    parser.add_argument("--directory", default="Builds/Web")
    args = parser.parse_args()
    root = Path(args.directory).resolve()
    if not (root / "index.html").exists():
        parser.error(f"No Unity Web build at {root}")
    handler = lambda *a, **kw: UnityHandler(*a, directory=str(root), **kw)
    print(f"Serving {root} at http://127.0.0.1:{args.port}", flush=True)
    ThreadingHTTPServer(("127.0.0.1", args.port), handler).serve_forever()
