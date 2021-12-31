/// <binding BeforeBuild='default' />
var gulp = require('gulp');
const { series } = require('gulp');
var del = require('del');
var rename = require('gulp-rename');
var concat = require('gulp-concat');
var cleanCss = require('gulp-clean-css');
var uglify = require('gulp-uglify-es').default;

var nodeRoot = './node_modules/';
var kendoRoot = 'app/js/kendo/';
var targetPath = './wwwroot/';

var paths = {
    scripts: ['app/scripts/**/*.js', 'app/scripts/**/*.map']
};

var css = [
    
];
var cssMin = [
    
];
var vendorJs = [
   
];
var vendorJsMin = [
   
];

var kendoJs = [
 
];

var kendoJsMin = [
  
];


function clean(cb) {
    del(['wwwroot/css/fonts/**/*', 'wwwroot/css/**/*', 'wwwroot/js/**/*']);
    cb();
}

function defaultTask(cb) {

    gulp.src('app/css/style.bundle.css').pipe(gulp.dest(targetPath + 'css'));
    gulp.src('app/css/plugins.bundle.css').pipe(gulp.dest(targetPath + 'css'));
    
    gulp.src('app/js/scripts.bundle.js').pipe(gulp.dest(targetPath + 'js'));
    gulp.src('app/js/plugins.bundle.js').pipe(gulp.dest(targetPath + 'js'));

    cb();
}

function release(cb) {
    
    cb();
}

exports.clean = clean;
exports.default = defaultTask;
exports.release = series(defaultTask, release);
