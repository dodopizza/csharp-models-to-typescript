const path = require("path");

const flatten = arr => arr.reduce((a, b) => a.concat(b), []);

const collectionRegex = /^(?:I?List|IReadOnlyList|IEnumerable|ICollection|HashSet)<([\w\d]+)>\?*$/;
const dictionaryRegex = /^I?Dictionary<([\w\d]+),\s?([\w\d]+)>\?*$/;

const defaultTypeTranslations = {
  int: "number",
  double: "number",
  float: "number",
  Int32: "number",
  Int64: "number",
  short: "number",
  long: "number",
  decimal: "number",
  bool: "boolean",
  DateTime: "string",
  DateTimeOffset: "string",
  Guid: "string",
  dynamic: "any",
  object: "any"
};

const createConverter = config => {
  const typeTranslations = Object.assign(
    {},
    defaultTypeTranslations,
    config.customTypeTranslations
  );

  const convert = json => {
    const content = json.map(file => {
      const filename = path.relative(process.cwd(), file.FileName);

      return flatten([
        ...file.Models.map(model => convertModel(model)),
        ...file.Enums.map(enum_ => convertEnum(enum_, filename))
      ]).join("\n");
    });

    const filteredContent = content.filter(x => x.length > 0);

    return filteredContent.join("\n");
  };

  const convertModel = model => {
    const rows = [];
    const members = [...model.Fields, ...model.Properties];
    const baseClasses = model.BaseClasses
      ? ` extends ${model.BaseClasses}`
      : "";

    if (members.length > 0) {
      rows.push(`export interface ${model.ModelName}${baseClasses} {`);
      members.forEach(member => {
        rows.push(convertProperty(member));
      });

      rows.push(`}\n`);
    }

    return rows;
  };

  const convertEnum = enum_ => {
    const rows = [];

    rows.push(`export enum ${enum_.Identifier} {`);

    enum_.Items.forEach(item => {
      rows.push(`\t${item.Name} = ${item.Value},`);
    });

    rows.push(`}\n`);

    return rows;
  };

  const convertProperty = property => {
    const optional = property.Type.endsWith("?");
    const collection = property.Type.match(collectionRegex);
    const dictionary = property.Type.match(dictionaryRegex);
    const identifier = convertIdentifier(
      optional
        ? `${property.Identifier.split(" ")[0]}?`
        : property.Identifier.split(" ")[0]
    );

    let type;

    if (collection) {
      type = `${convertType(collection[1])}[]`;
    } else if (dictionary) {
      type = `{ [index: ${convertType(dictionary[1])}]: ${convertType(
        dictionary[2]
      )} }`;
    } else {
      type = convertType(
        optional
          ? property.Type.slice(0, property.Type.length - 1)
          : property.Type
      );
    }

    return `\t${identifier}: ${type}`;
  };

  const convertIdentifier = identifier =>
    config.camelCase
      ? identifier[0].toLowerCase() + identifier.substring(1)
      : identifier;
  const convertType = type =>
    type in typeTranslations ? typeTranslations[type] : type;

  return convert;
};

module.exports = createConverter;
