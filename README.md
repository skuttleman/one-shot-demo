# One Shot

A top-down stealth game for Unity3D.

## Dependencies

- Unity v2021.3.12f1
- AWS CLI tool
- Babashka

## Development

### Pulling Project

```bash
$ git clone https://github.com/skuttleman/one-shot.git
$ cd one-shot
$ bb go s3 pull
```

### Pushing Project

Make sure to push S3 assets as well.

```bash
$ git commit
$ git push
$ bb go s3 push
```
