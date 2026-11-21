#!/usr/bin/env python3
"""Run the project's tests through an already-open Unity Pipeline editor."""
import json
from pathlib import Path
import subprocess
import time

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "docs" / "verification" / "results"
OUT.mkdir(parents=True, exist_ok=True)


def command(name, *args):
    run = subprocess.run(
        ["unity", "--json", "command", "--timeout", "120", name, *args],
        cwd=ROOT, text=True, capture_output=True, timeout=150,
    )
    try:
        response = json.loads(run.stdout)
    except json.JSONDecodeError:
        raise RuntimeError(run.stdout + run.stderr)
    return response


def verify(mode, filename):
    started = command("test run", "--mode", mode, "--async-tests")
    if not started.get("success"):
        raise RuntimeError(started)
    deadline = time.monotonic() + 180
    while time.monotonic() < deadline:
        response = command("test status", "--detail", "full")
        result = (response.get("data") or {}).get("result") or {}
        if result.get("status") == "completed":
            (OUT / filename).write_text(json.dumps(response, indent=2) + "\n")
            summary = result.get("summary", {})
            if not response.get("success") or summary.get("total", 0) == 0 or summary.get("failed", 0):
                raise RuntimeError(response)
            print(f"{mode}: {summary}", flush=True)
            return
        time.sleep(1)
    raise TimeoutError(f"{mode} tests did not complete")


if __name__ == "__main__":
    verify("editor", "editmode-results.json")
    verify("playmode", "playmode-results.json")
