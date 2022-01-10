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


function clean(cb) {
    del(['wwwroot/css/fonts/**/*', 'wwwroot/css/**/*', 'wwwroot/js/**/*']);
    cb();
}

function defaultTask(cb) {

    gulp.src('app/css/style.bundle.css').pipe(gulp.dest(targetPath + 'css'));
    gulp.src('app/plugins/global/plugins.bundle.css').pipe(gulp.dest(targetPath + 'css'));
    
    gulp.src('app/js/scripts.bundle.js').pipe(gulp.dest(targetPath + 'js'));
    gulp.src('app/plugins/global/plugins.bundle.js').pipe(gulp.dest(targetPath + 'js'));

    gulp.src('app/plugins/global/fonts/**/*').pipe(gulp.dest(targetPath + 'css/fonts/'));

    gulp.src('app/js/pages/**/*').pipe(gulp.dest(targetPath + 'js/pages/'));

    cb();
}

function release(cb) {
    
    cb();
}

exports.clean = clean;
exports.default = defaultTask;
exports.release = series(defaultTask, release);
