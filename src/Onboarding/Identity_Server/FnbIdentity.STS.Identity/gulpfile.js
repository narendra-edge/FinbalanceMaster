import gulp from 'gulp';
import * as del from 'del';
import concat from 'gulp-concat';
import uglify from 'gulp-uglify';
import gulpSass from 'gulp-sass'; 
import nodeSass from "node-sass";
import minifyCSS from 'gulp-clean-css';

const sass = gulpSass(nodeSass);
const paths = {
	styles: {
		src: [
			'./node_modules/bootstrap/dist/css/bootstrap.css',
			'./node_modules/open-iconic/font/css/open-iconic-bootstrap.css',
			'./node_modules/font-awesome/css/font-awesome.css',
			'./node_modules/cookieconsent/build/cookieconsent.min.css'
		],
		dest: 'wwwroot/dist/css'
	},
	scripts: {
		src: [
			'./node_modules/jquery/dist/jquery.js',
			'./node_modules/jquery-validation/dist/jquery.validate.js',
			'./node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js',
			'./node_modules/popper.js/dist/umd/popper.js',
			'./node_modules/bootstrap/dist/js/bootstrap.js',
			'./node_modules/cookieconsent/src/cookieconsent.js',
			'./node_modules/holderjs/holder.js',
			'./Scripts/App/components/Menu.js',
			'./Scripts/App/components/Language.js',
			'./Scripts/App/components/Theme.js',
			'./Scripts/App/components/CookieConsent.js'
		],
		dest: 'wwwroot/dist/js'
	},
	fonts: {
		src: [
			'./node_modules/font-awesome/fonts/**',
			'./node_modules/open-iconic/font/fonts/**'
		],
		dest: 'wwwroot/dist/fonts'
	},
	themes: {
		src: 'node_modules/bootswatch/dist/**/bootstrap.min.css',
		dest: 'wwwroot/dist/css/theme'
	}
};




gulp.task('scripts', () => {
	return gulp
		.src(paths.scripts.src)
		.pipe(concat('bundle.min.js'))
		.pipe(uglify())
		.pipe(gulp.dest(paths.scripts.dest));
});

gulp.task('fonts', () => {
	return gulp
		.src(paths.fonts.src)
		.pipe(gulp.dest(paths.fonts.dest));
});

gulp.task('sass', () => {
	return gulp
		.src('Styles/web.scss')
		.pipe(sass())
		.on('error', sass.logError)
		.pipe(gulp.dest(paths.styles.dest));
});
gulp.task('sassMin', () => {
	return gulp
		.src('Styles/web.scss')
		.pipe(sass())
		.on('error', sass.logError)
		.pipe(minifyCSS())
		.pipe(concat('web.min.css'))
		.pipe(gulp.dest(paths.styles.dest));
});

gulp.task('styles', () => {
	return gulp
		.src(paths.styles.src)
		.pipe(minifyCSS())
		.pipe(concat('bundle.min.css'))
		.pipe(gulp.dest(paths.styles.dest));
});

gulp.task('theme', () => {
	return gulp
		.src(paths.themes.src)
		.pipe(gulp.dest(paths.themes.dest));
});

gulp.task('watch', () => {
	gulp.watch(paths.styles.src, gulp.series('styles'));
	gulp.watch('src/styles/**/*.scss', gulp.series('sass'));
	gulp.watch(paths.fonts.src, gulp.series('fonts'));
	gulp.watch('Styles/**/*.scss');
});

gulp.task('default', gulp.parallel('sass', 'scripts', 'styles', 'fonts', 'watch'));

gulp.task('clean', () => {
	return del([paths.styles.dest, paths.scripts.dest, paths.fonts.dest]);
});