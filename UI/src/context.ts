import { createContext } from "react";

import { defaultLocale } from "localisations";

const LocaleContext = createContext(defaultLocale);

export {
	LocaleContext
};