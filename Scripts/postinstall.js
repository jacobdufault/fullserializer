var mkdirp = require('mkdirp');
var path = require('path');
var ncp = require('ncp');

// Package name
var package = "TEMPLATE";

// Paths
var src = path.join(__dirname, '..', 'Assets', 'FullSerializer', 'Source');
var dir = path.join(__dirname, '..', '..', '..', 'Assets', 'packages', 'fullserializer');

// Create folder if missing
mkdirp(dir, function (err) {
  if (err) {
    console.error(err)
    process.exit(1);
  }

  // Copy files
  ncp(src, dir, function (err) {
    if (err) {
      console.error(err);
      process.exit(1);
    }
  });
});
