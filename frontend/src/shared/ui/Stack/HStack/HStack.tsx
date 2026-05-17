import { forwardRef } from 'react';
import { Flex, type FlexProps } from '../Flex/Flex';

type HStackProps = Omit<FlexProps, 'direction'>;

export const HStack = forwardRef<HTMLDivElement, HStackProps>((props, ref) => {
  return <Flex ref={ref} direction="row" {...props} />;
});
