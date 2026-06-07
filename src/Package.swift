// swift-tools-version: 5.9

import PackageDescription

let package = Package(
	name: "Apothecary",
	products: [
		.library(
			name: "Apothecary",
			type: .dynamic,
			targets: ["Apothecary"]
		)
	],
	dependencies: [
        .package(url: "https://github.com/migueldeicaza/SwiftGodot", branch: "main"),
		.package(url: "https://github.com/apple/swift-collections.git", branch: "release/1.1"),
		.package(url: "https://github.com/apple/swift-algorithms", from: "1.2.0"),
		.package(url: "https://github.com/LebJe/TOMLKit.git", from: "0.5.0")
    ],
	targets: [
		.target(
			name: "Apothecary",
			dependencies: [
				"SwiftGodot",
				.product(name: "Collections",  package: "swift-collections"),
				.product(name: "Algorithms", package: "swift-algorithms"),
				.product(name: "TOMLKit", package: "TOMLKit")
			],
			path: "Sources",
			swiftSettings: [
				.unsafeFlags(["-Xfrontend", "-warn-long-expression-type-checking=200"])
			]
		)
	]
)