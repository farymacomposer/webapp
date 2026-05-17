import { Flex, type FlexProps } from '../Flex/Flex';
import { forwardRef } from 'react';

type HStackProps = Omit<FlexProps, 'direction'>;

export const HStack = forwardRef<HTMLDivElement, HStackProps>((props, ref) => {
  return <Flex ref={ref} direction="row" {...props} />;
});
